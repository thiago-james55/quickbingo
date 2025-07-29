import { Component, OnInit } from '@angular/core';
import { SignalrService } from '../../service/signalr.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { AudioService } from '../../service/audio.service';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainComponent implements OnInit {

  showModal = false;
  playerName: string = '';
  bingoCard: number[] = [];
  playerList: string[] = [];
  numberDrawn: number | null = null;
  drawnNumbers: number[] = [];

  constructor(private signalrService: SignalrService, public router: Router, public audioService: AudioService) { }

  async ngOnInit(): Promise<void> {

    await this.signalrService.startConnection();

    let storedName: string | null = null;

    if (typeof window !== 'undefined' && window.localStorage) {
      storedName = localStorage.getItem('playerName');
    }

    if (storedName) {
      this.playerName = storedName;
      this.bingoCard = await this.signalrService.invokeRegisterPlayer(this.playerName);
      this.showModal = false;
    } else {
      this.showModal = true;
    }

    this.signalrService.playerList$.subscribe(list => {
      this.playerList = list;
    });

    this.signalrService.numberDrawn$.subscribe(num => {
      if (num !== null) {
        this.numberDrawn = num;
        if (!this.drawnNumbers.includes(num)) {
          this.drawnNumbers.push(num);
          if (this.bingoCard.includes(num)) {
            this.audioService.play("win");
          } else {
            this.audioService.play("numberDraw");
          }

        }
      }
    });

    this.drawnNumbers = await this.signalrService.invokeGetSortedNumbers() ?? [];
  }

  async confirmName() {
    if (!this.playerName.trim()) {
      alert('Digite seu nome');
      return;
    }

    try {
      localStorage.setItem('playerName', this.playerName);
      this.bingoCard = await this.signalrService.invokeRegisterPlayer(this.playerName);
      console.log('Cartela recebida:', this.bingoCard);

      if (!this.bingoCard || this.bingoCard.length === 0) {
        alert('Erro ao registrar jogador. Tente novamente.');
        return;
      }

      this.showModal = false;
    } catch (error) {
      console.error('Erro no confirmName:', error);
      alert('Erro ao conectar com o servidor.');
    }
  }

  cancelName() {
    this.showModal = false;
  }

  openModal() {
    this.showModal = true;
  }

  public getFormattedCard(): (number | string)[][] {
    if (!this.bingoCard || this.bingoCard.length !== 24) {
      return [];
    }

    const card: (number | string)[][] = [];
    const numbers = [...this.bingoCard]; // cópia do array original

    for (let row = 0; row < 5; row++) {
      const rowData: (number | string)[] = [];
      for (let col = 0; col < 5; col++) {
        if (row === 2 && col === 2) {
          rowData.push('FREE');
        } else {
          rowData.push(numbers.shift() ?? ''); // Correção aqui
        }
      }
      card.push(rowData);
    }

    return card;
  }

  isNumberMarked(cell: number | string): boolean {
    if (typeof cell !== 'number') return false;
    return this.drawnNumbers.includes(cell);
  }


  startGame() {
    this.signalrService.invokeStartGame();
  }

  showStartButton(): boolean {
    return this.router.url.includes('/admin');
  }

}

import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SignalrService {
  private hubConnection!: signalR.HubConnection;

  public numberDrawn$ = new BehaviorSubject<number | null>(null);
  public winner$ = new BehaviorSubject<string | null>(null);
  public playerList$ = new BehaviorSubject<string[]>([]);

  constructor() {}

  public async startConnection(): Promise<void> {
  this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('http://' + window.location.hostname + ':5033/bingoHub')
    .withAutomaticReconnect()
    .build();

  try {
    await this.hubConnection.start();
    console.log('SignalR conectado');
    this.registerListeners();
  } catch (err) {
    console.error('Erro SignalR:', err);
  }
}


  private registerListeners(): void {
    this.hubConnection.on('NumberDrawn', (number: number) => {
      this.numberDrawn$.next(number);
    });

    this.hubConnection.on('Winner', (name: string) => {
      this.winner$.next(name);
    });

    this.hubConnection.on('PlayerListUpdated', (players: string[]) => {
      this.playerList$.next(players);
    });
  }

  public invokeRegisterPlayer(name: string): Promise<number[]> {
    return this.hubConnection.invoke('RegisterPlayer', name);
  }

  public invokeResetGame(): void {
    this.hubConnection.invoke('Reset');
  }
  public invokeGetSortedNumbers(): Promise<number[]> {
    return this.hubConnection.invoke('GetSortedNumbers');
  }

  public invokeStartGame(): void {
    this.hubConnection.invoke('StartGame');
  }

  public invokeResumeGame(): void {
    this.hubConnection.invoke('ResumeGame');
  }

  public invokePauseGame(): void {
    this.hubConnection.invoke('PauseGame');
  }

  public invokeWinGame(card: number[]): void {
    this.hubConnection.invoke('WinGame', card);
  }

  public stopConnection(): void {
    this.hubConnection.stop();
  }
}

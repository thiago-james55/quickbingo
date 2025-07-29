import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AudioService {
  private isBrowser: boolean;
  private sounds: { [key: string]: HTMLAudioElement } = {};

  constructor() {
    this.isBrowser = typeof window !== 'undefined' && typeof Audio !== 'undefined';

    if (this.isBrowser) {
      this.loadSound('numberDraw', 'assets/sounds/number-draw.mp3');
      this.loadSound('startGame', 'assets/sounds/start-game.mp3');
      this.loadSound('win', 'assets/sounds/win.mp3');
      this.loadSound('error', 'assets/sounds/error.mp3');
    }
  }

  private loadSound(name: string, path: string): void {
    if (this.isBrowser) {
      this.sounds[name] = new Audio(path);
    }
  }

  play(name: string): void {
    if (!this.isBrowser) return;

    const sound = this.sounds[name];
    if (sound) {
      sound.currentTime = 0;
      sound.play().catch(err => console.error('Erro ao tocar o som:', err));
    } else {
      console.warn(`Som '${name}' não encontrado.`);
    }
  }
}

import { Component } from '@angular/core';
import { MainLayout } from './shell/main-layout/main-layout';

@Component({
  selector: 'app-root',
  imports: [MainLayout],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = 'Endpoint Engineering';
}

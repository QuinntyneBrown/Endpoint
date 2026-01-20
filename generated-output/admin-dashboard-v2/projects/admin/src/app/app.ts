import { Component } from '@angular/core';
import { MainLayout } from './shell/main-layout/main-layout';

@Component({
  selector: 'admin-root',
  imports: [MainLayout],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = 'admin';
}

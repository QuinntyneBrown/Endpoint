import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppHeader } from 'endpoint-engineering-components';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, AppHeader],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
export class MainLayout {
  protected readonly appTitle = 'Endpoint Engineering';
}

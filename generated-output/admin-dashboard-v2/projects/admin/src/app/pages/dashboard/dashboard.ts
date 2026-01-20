import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HelloWorldTile } from '../../components/hello-world-tile/hello-world-tile';

@Component({
  selector: 'admin-dashboard',
  imports: [CommonModule, HelloWorldTile],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard {
}

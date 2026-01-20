import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'admin-hello-world-tile',
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './hello-world-tile.html',
  styleUrl: './hello-world-tile.scss'
})
export class HelloWorldTile {
}

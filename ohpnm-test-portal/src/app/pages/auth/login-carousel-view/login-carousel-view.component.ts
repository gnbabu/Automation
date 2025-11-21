import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Carousel } from 'bootstrap';

@Component({
  selector: 'app-login-carousel-view',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login-carousel-view.component.html',
  styleUrls: ['./login-carousel-view.component.css'],
})
export class LoginCarouselViewComponent {
  ngAfterViewInit(): void {
    const element = document.getElementById('loginCarousel');
    if (element) {
      new Carousel(element, {
        interval: 4500,
        ride: 'carousel',
        pause: false,
        wrap: true,
      });
    }
  }
}

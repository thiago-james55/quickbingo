import { Component } from '@angular/core';
import { HeaderComponent } from "../../layouts/header/header.component";
import { FooterComponent } from '../../layouts/footer/footer.component';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [HeaderComponent, FooterComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent {

}

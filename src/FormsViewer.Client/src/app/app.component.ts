import { Component, OnInit } from '@angular/core';
import { NgScService } from '@speak/ng-sc';
import { SciLogoutService } from '@speak/ng-sc/logout';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  
  constructor(
    private ngScService: NgScService,
    public logoutService: SciLogoutService
  ) { }

  ngOnInit() {
    // Call init to first fetch context, then translations
    this.ngScService.init();
  }
}

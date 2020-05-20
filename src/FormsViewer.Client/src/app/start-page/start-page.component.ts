import { Component, OnInit, ValueProvider } from '@angular/core';
import { FormsViewerService } from '../formsviewer.service';
import { NavigationModel, ItemWebApiResponse, NavigationWrapper, ItemModel, ItemWebApiResult } from '../navigation';
import { SciLogoutService } from '@speak/ng-sc/logout';
import { analyzeAndValidateNgModules } from '@angular/compiler';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-start-page',
  templateUrl: './start-page.component.html',
  styleUrls: ['./start-page.component.scss']
})
export class StartPageComponent implements OnInit {

  isNavigationShown: boolean;
  isActive: boolean;
  isEditing = false;
  itemWebApiResponse: ItemWebApiResult;
  list2: NavigationWrapper;
  query = '';
  displayTree: boolean;
  selectedForm = "";
  databases = ["master", "core", "web"];
  isLoading: boolean;
  queryResult: any;
  errorMessage: string;
  startDate:Date;
  endDate:Date;

  constructor(private formsViewerService: FormsViewerService, 
    public logoutService: SciLogoutService,
    private route: ActivatedRoute,    
    private router: Router) { this.displayTree = false; }

  ngOnInit() {
    let itemId = this.route.snapshot.queryParamMap.get('form_id');
    
    this.initFormsDropdown();

    if(itemId){
      this.selectedForm = itemId;
      this.loadForms();
    }
  }

  
  forms:any;
  initFormsDropdown() {
    this.list2 = { items: [] };
    var result = this.formsViewerService.fetchForms().subscribe({
      next: response => {
        this.forms = response;
      }
    });
  }

  formEntries:any;
  formStatistics:any;
  statisticsLoading = false;
  loadForms(){
    this.isLoading = true;
    this.statisticsLoading = true;
    var result = this.formsViewerService.fetchFormDetail(this.selectedForm, this.startDate, this.endDate).subscribe({
      next: response => {
        this.formEntries = response;
        this.isLoading = false;
      }
    });

    this.formsViewerService.fetchFormStatistics(this.selectedForm, this.startDate, this.endDate).subscribe({
      next: response => {
        this.formStatistics = response;
        this.isLoading = false;
        this.statisticsLoading = false;
      }
    });

  }
}

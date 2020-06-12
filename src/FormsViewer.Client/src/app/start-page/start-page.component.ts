import { Component, OnInit, ValueProvider } from '@angular/core';
import { FormsViewerService } from '../formsviewer.service';
import { NavigationModel, ItemWebApiResponse, NavigationWrapper, ItemModel, ItemWebApiResult } from '../navigation';
import { SciLogoutService } from '@speak/ng-sc/logout';
import { analyzeAndValidateNgModules } from '@angular/compiler';
import { ActivatedRoute, Router } from '@angular/router';
import * as Chart from 'chart.js'
import { ScDialogService } from '@speak/ng-bcl/dialog';

@Component({
  selector: 'app-start-page',
  templateUrl: './start-page.component.html',
  styleUrls: ['./start-page.component.scss']
})
export class StartPageComponent implements OnInit {

  canvas: any;
  ctx: any;

  isNavigationShown: boolean;
  isActive: boolean;
  isEditing = false;
  itemWebApiResponse: ItemWebApiResult;
  list2: NavigationWrapper;
  query = '';
  displayTree: boolean;
  selectedForm: any;
  selectedFormId = "";
  databases = ["master", "core", "web"];
  isLoading: boolean;
  queryResult: any;
  errorMessage: string;
  startDate: Date;
  endDate: Date;

  parent: any;

  constructor(private formsViewerService: FormsViewerService,
    public logoutService: SciLogoutService,
    private route: ActivatedRoute,
    private router: Router,
    public dialogService: ScDialogService) {
    this.displayTree = false;
    this.parent = this;
  }

  ngOnInit() {
    let itemId = this.route.snapshot.queryParamMap.get('form_id');

    this.initFormsDropdown();

    if (itemId) {
      this.selectedFormId = itemId;
      this.loadForms();
    }
  }

  loadChart() {
    if (!this.settings.XDbEnabled) {
      return;
    }

    if (this.settings && this.settings.XDbEnabled == true) {
      this.formsViewerService.fetchFormStatistics(this.selectedFormId, this.startDate, this.endDate).subscribe({
        next: response => {
          this.formStatistics = response;
          this.isLoading = false;
          this.statisticsLoading = false;

          this.canvas = document.getElementById('myChart');
          this.ctx = this.canvas.getContext('2d');
          let myChart = new Chart(this.ctx, {
            type: 'bar',
            data: {
              labels: ["Success submits", "Submit Count", "Errors", "Visits", "DropOuts"],
              datasets: [{
                label: 'Form statistics',
                data: [this.formStatistics.SuccessSubmits, this.formStatistics.SubmitsCount, this.formStatistics.Errors, this.formStatistics.Visits, this.formStatistics.Dropouts],
                backgroundColor: [
                  'rgba(66, 245, 69)',
                  'rgba(53, 74, 53)',
                  'rgba(255, 99, 132, 1)',
                  'rgba(54, 162, 235, 1)',
                  'rgba(255, 206, 86, 1)'
                ],
                borderWidth: 1
              }]
            },
            options: {
              scales: {
                yAxes: [{
                  display: true,
                  ticks: {
                    suggestedMin: 0,
                    stepSize: 1

                  }
                }]
              },
              title: {
                display: true,
                text: 'Form Analytics'
              }
              , legend: {
                display: false
              }
            }
          }
          );
        }
      });
    }


  }

  exportType: any;
  exportFields: any;
  blob: any;
  alertAndClose() {
    this.exportFields = this.selectedOptions();

    this.formsViewerService.exportFormData(this.selectedFormId, this.startDate, this.endDate, this.exportType, this.exportFields)
      .subscribe({
        next: response => {
          this.blob = response;
          let filename = 'export.' + this.getExtension(this.exportType);

          var link = document.createElement("a");
          // Browsers that support HTML5 download attribute
          if (link.download !== undefined) {
            var url = URL.createObjectURL(response);
            link.setAttribute("href", url);
            link.setAttribute("download", filename);
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
          }
        }
      });
  }

  getExtension(exportType: any) {
    if (exportType == 'excel') {
      return 'xlsx';
    }

    return exportType;
  }
  selectedOptions() { // right now: ['1','3']
    return this.options
      .filter(opt => opt.checked)
      .map(opt => opt.value)
  }

  forms: any;
  settings: any;
  initFormsDropdown() {
    this.list2 = { items: [] };
    this.formsViewerService.fetchForms().subscribe({
      next: response => {
        this.forms = response;
      }
    });


    this.formsViewerService.settings().subscribe({
      next: response => {
        this.settings = response;
      }
    });
  }



  formEntries: any;
  formStatistics: any;
  statisticsLoading = false;
  options = [];
  loadForms() {
    this.selectedFormId = this.selectedForm;
    this.isLoading = true;
    this.statisticsLoading = true;
    var result = this.formsViewerService.fetchFormDetail(this.selectedFormId, this.startDate, this.endDate).subscribe({
      next: response => {
        this.formEntries = response;
        this.isLoading = false;
        this.options = [];
        for (var i = 0; i < this.formEntries.Headers.length; i++) {
          this.options.push({ name: this.formEntries.Headers[i], value: this.formEntries.Headers[i], checked: true });
        }
      }
    });

  }
}

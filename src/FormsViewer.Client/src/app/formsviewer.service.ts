import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class FormsViewerService {

  private baseUrl: string;
  constructor(private httpClient: HttpClient) {
    this.baseUrl = "/sitecore/api/ssc/formsviewerapi";

    // if (window.location.origin.indexOf('localhost') > -1) {
    //   this.baseUrl = "https://forms.local/sitecore/api/ssc/formsviewerapi";
    // }
    
  }

  fetchForms(){
    return this.httpClient.get(this.baseUrl+"/forms?sc_site=shell");
  }

  fetchFormDetail(formId:string, startDate:Date, endDate:Date) {
    return this.httpClient.post(this.baseUrl+'/detail?sc_site=shell', { FormId:formId, StartDate:startDate, EndDate:endDate});
  }

  fetchFormStatistics(formId:string, startDate:Date, endDate:Date) {
    return this.httpClient.post(this.baseUrl+'/statistics?sc_site=shell', { FormId:formId, StartDate:startDate, EndDate:endDate});
  }

  settings(){
    return this.httpClient.get(this.baseUrl+'/settings');
  }

  exportFormData(formId:string, startDate:Date, endDate:Date, exportOption:string, fields:any){
    return this.httpClient.post(this.baseUrl+'/exportformdata?sc_site=shell', { FormId:formId, StartDate:startDate, EndDate:endDate, ExportOption:exportOption, Fields:fields}, {responseType:'blob'});
  }
}

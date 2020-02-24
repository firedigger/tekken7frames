import { Component, Inject, OnInit } from '@angular/core';
import { Client } from '../client.generated';
import { PageSettingsModel, FilterSettingsModel } from '@syncfusion/ej2-angular-grids';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent implements OnInit {

  private moves: object[];
  public pageSettings: PageSettingsModel;
  public filterOptions: FilterSettingsModel;

  constructor(private client: Client) {
  }

  ngOnInit(): void {
    this.client.moves_Get().subscribe(moves => {
      this.moves = moves;
    });
    this.pageSettings = { pageSize: 25 };
    this.filterOptions = { type: 'Excel' };
  }

}
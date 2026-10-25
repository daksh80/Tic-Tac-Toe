import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-search',
  standalone: true,
  templateUrl: './search.component.html',
  styleUrl: './search.component.css'
})
export class SearchComponent {
  @Output() searchChange = new EventEmitter<string>();

  /** Emits the current text from the search input. */
  public onSearch(event: Event): void {
    const input = event.target;
    if (input instanceof HTMLInputElement) {
      this.searchChange.emit(input.value);
    }
  }
}

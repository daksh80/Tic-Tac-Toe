import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/small-game-component/small-game-component.component').then(
        (component) => component.SmallGameComponentComponent
      )
  }
];

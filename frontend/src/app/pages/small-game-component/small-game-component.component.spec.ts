import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SmallGameComponentComponent } from './small-game-component.component';

describe('SmallGameComponentComponent', () => {
  let component: SmallGameComponentComponent;
  let fixture: ComponentFixture<SmallGameComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SmallGameComponentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SmallGameComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

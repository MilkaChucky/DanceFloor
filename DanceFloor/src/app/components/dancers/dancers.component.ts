import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DoCheck,
  Input,
  IterableDiffer,
  IterableDiffers,
  OnInit
} from '@angular/core';
import { DanceClassService } from '../../services/dance-class.service';
import { UserService } from '../../services/user.service';
import { UserInfo } from '../../models/user-info';
import { AuthService } from '../../services/auth.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-dancers',
  templateUrl: './dancers.component.html',
  styleUrls: ['./dancers.component.css']
})
export class DancersComponent implements OnInit, DoCheck {
  @Input() danceClassId: string;
  @Input() dancers: UserInfo[];

  private iterableDiffer: IterableDiffer<UserInfo>;

  constructor(
    private readonly authService: AuthService,
    private readonly userService: UserService,
    private readonly danceClassService: DanceClassService,
    private readonly differs: IterableDiffers,
    private readonly cd: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.iterableDiffer = this.differs
      .find(this.dancers)
      .create((i, value) => value.id);
  }

  ngDoCheck(): void {
    const changes = this.iterableDiffer.diff(this.dancers);

    if (changes) {
      this.cd.markForCheck();
    }
  }

  hasAlreadyApplied(): boolean {
    const currentUser = this.authService.currentUser;
    return this.dancers.some(dancer => dancer.id === currentUser.id);
  }

  apply(): void {
    this.danceClassService.applyForClass(this.danceClassId)
      .subscribe({ complete: () => console.log('Applied') });
  }

  cancel(): void {
    this.danceClassService.cancelApplication(this.danceClassId)
      .subscribe({ complete: () => console.log('Cancelled') });
  }
}

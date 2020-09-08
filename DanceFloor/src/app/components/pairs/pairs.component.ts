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
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { UserInfo } from '../../models/user-info';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-pairs',
  templateUrl: './pairs.component.html',
  styleUrls: ['./pairs.component.css']
})
export class PairsComponent implements OnInit, DoCheck {
  @Input() danceClassId: string;
  @Input() pairs: UserInfo[][];

  private iterableDiffer: IterableDiffer<UserInfo[]>;

  constructor(
    private readonly authService: AuthService,
    private readonly userService: UserService,
    private readonly danceClassService: DanceClassService,
    private readonly differs: IterableDiffers,
    private readonly cd: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.iterableDiffer = this.differs
      .find(this.pairs)
      .create((i, value) => value.map(userInfo => userInfo.id).join(' '));
  }

  ngDoCheck(): void {
    const changes = this.iterableDiffer.diff(this.pairs);

    if (changes) {
      this.cd.markForCheck();
    }
  }

  joinNames(pair: UserInfo[]): string {
    return pair.map(dancer => dancer.name).join(' - ');
  }

  hasAlreadyApplied(): boolean {
    const currentUser = this.authService.currentUser;

    if (this.pairs.length <= 0) {
      return false;
    }

    return this.pairs
      .reduce((acc, val) => [...acc, ...val])
      .some(dancer => dancer.id === currentUser.id);
  }

  apply(pairId?: string): void {
    this.danceClassService.applyForClass(this.danceClassId, pairId)
      .subscribe({ complete: () => console.log('Applied') });
  }

  cancel(): void {
    this.danceClassService.cancelApplication(this.danceClassId)
      .subscribe({ complete: () => console.log('Cancelled') });
  }
}

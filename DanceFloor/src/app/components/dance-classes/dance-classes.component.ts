import { AfterViewInit, Component, OnInit } from '@angular/core';
import { DanceClassService } from '../../services/dance-class.service';
import { DanceClasses } from '../../models/dance-hall';
import { DanceClass } from '../../models/dance-class';
import { GroupDanceClass } from '../../models/group-dance-class';
import { BallroomDanceClass } from '../../models/ballroom-dance-class';
import { duration, utc } from 'moment';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { WeekDay } from '@angular/common';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-dance-classes',
  templateUrl: './dance-classes.component.html',
  styleUrls: ['./dance-classes.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({ height: '0', minHeight: '0' })),
      state('expanded', style({ height: '*' })),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)'))
    ])
  ]
})
export class DanceClassesComponent implements OnInit, AfterViewInit {
  danceClasses: DanceClasses = [];
  displayedColumns = ['name', 'teacher', 'address', 'room', 'dayOfWeek', 'duration', 'dancers'];
  expanded: (DanceClass | GroupDanceClass | BallroomDanceClass) | null;

  constructor(
    private readonly userService: UserService,
    private readonly danceClassService: DanceClassService
  ) { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.danceClassService.getClasses()
      .subscribe(danceClasses => { this.danceClasses = danceClasses; });
  }

  getNumberOfDancers(danceClass: (DanceClass | GroupDanceClass | BallroomDanceClass)): number {
    if ('pairs' in danceClass) {
      return danceClass.pairs.reduce((acc, val) => [...acc, ...val]).length;
    } else if ('dancers' in danceClass) {
      return danceClass.dancers.length;
    } else {
      return 0;
    }
  }

  dayOfWeekAsString(danceClass: (DanceClass | GroupDanceClass | BallroomDanceClass)): string {
    return WeekDay[danceClass.dayOfWeek];
  }

  timeIntervalAsString(danceClass: (DanceClass | GroupDanceClass | BallroomDanceClass)): string {
    const startsAt = utc(duration(danceClass.startsAt).asMilliseconds()).format('HH:mm');
    const endsAt = utc(duration(danceClass.endsAt).asMilliseconds()).format('HH:mm');
    return `${startsAt} - ${endsAt}`;
  }
}

import { DurationInputObject } from 'moment';
import { WeekDay } from '@angular/common';
import { UserInfo } from './user-info';

export interface DanceClass {
  id: string;
  name: string;
  dayOfWeek: WeekDay;
  teacher: string | UserInfo;
  startsAt: DurationInputObject;
  endsAt: DurationInputObject;
  address?: string;
  room?: string;
}

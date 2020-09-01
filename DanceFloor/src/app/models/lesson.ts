import { DurationInputObject } from 'moment';

export interface Lesson {
  id: string;
  teacher: string;
  startsAt: DurationInputObject;
  endsAt: DurationInputObject;
}

import { Lesson } from './lesson';
import { DanceLesson } from './dance-lesson';
import { BallroomDanceLesson } from './ballroom-dance-lesson';

export type Lessons = (Lesson | DanceLesson | BallroomDanceLesson)[];

export interface DanceHall {
  id: string;
  address: string;
  room: string;
  lessons: Lessons;
}

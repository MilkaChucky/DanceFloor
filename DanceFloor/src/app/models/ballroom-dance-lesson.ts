import { Lesson } from './lesson';

export interface BallroomDanceLesson extends Lesson {
  pairs: string[][];
}

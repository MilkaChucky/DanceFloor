import { DanceClass } from './dance-class';
import { UserInfo } from './user-info';

export interface BallroomDanceClass extends DanceClass {
  pairs: (string | UserInfo)[][];
}

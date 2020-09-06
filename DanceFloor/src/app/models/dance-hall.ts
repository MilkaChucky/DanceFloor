import { DanceClass } from './dance-class';
import { GroupDanceClass } from './group-dance-class';
import { BallroomDanceClass } from './ballroom-dance-class';

export type DanceClasses = (DanceClass | GroupDanceClass | BallroomDanceClass)[];

export interface DanceHall {
  id: string;
  address: string;
  room: string;
  classes: DanceClasses;
}

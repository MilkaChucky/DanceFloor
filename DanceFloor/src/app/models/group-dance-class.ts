import { DanceClass } from './dance-class';
import { UserInfo } from './user-info';

export interface GroupDanceClass extends DanceClass {
  dancers: (string | UserInfo)[];
}

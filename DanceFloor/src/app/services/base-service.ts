import { environment } from '../../environments/environment';
import { Observable, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

export abstract class BaseService {
    protected readonly backendUrl = environment.backendUrl;

    protected handleError(snackBar?: MatSnackBar): (error: any) => Observable<never> {
        return (error: any): Observable<never> => {
          if (snackBar && error.error && error.error.reason) {
            snackBar.open(error.error.reason, 'Close', {
              duration: 2000
            });
          }
          console.error(error);
          return throwError(error);
        };
    }
}

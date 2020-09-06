import { environment } from '../../environments/environment';
import { Observable, throwError } from 'rxjs';

export abstract class BaseService {
    protected readonly backendUrl = environment.backendUrl;

    protected handleError(): (error: any) => Observable<never> {
        return (error: any): Observable<never> => {
            console.error(error);
            return throwError(error);
        };
    }
}

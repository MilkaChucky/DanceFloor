import { environment } from '../../environments/environment';
import { Observable, throwError } from 'rxjs';

export abstract class BaseService {
    protected readonly backendUrl = environment.backendUrl;

    protected handleError(): (error: any) => Observable<any> {
        return (error: any): Observable<any> => {
            console.error(error);
            return throwError(error.reason);
        };
    }
}

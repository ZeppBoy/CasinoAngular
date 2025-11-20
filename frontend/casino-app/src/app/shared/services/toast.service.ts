import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title?: string;
  message: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  public toasts$: Observable<Toast[]> = this.toastsSubject.asObservable();

  show(toast: Omit<Toast, 'id'>) {
    const id = this.generateId();
    const newToast: Toast = { id, ...toast };
    
    const currentToasts = this.toastsSubject.value;
    this.toastsSubject.next([...currentToasts, newToast]);

    const duration = toast.duration || 5000;
    setTimeout(() => {
      this.remove(id);
    }, duration);
  }

  success(message: string, title?: string, duration?: number) {
    this.show({ type: 'success', message, title, duration });
  }

  error(message: string, title?: string, duration?: number) {
    this.show({ type: 'error', message, title, duration });
  }

  warning(message: string, title?: string, duration?: number) {
    this.show({ type: 'warning', message, title, duration });
  }

  info(message: string, title?: string, duration?: number) {
    this.show({ type: 'info', message, title, duration });
  }

  remove(id: string) {
    const currentToasts = this.toastsSubject.value;
    this.toastsSubject.next(currentToasts.filter(t => t.id !== id));
  }

  clear() {
    this.toastsSubject.next([]);
  }

  private generateId(): string {
    return `toast_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }
}

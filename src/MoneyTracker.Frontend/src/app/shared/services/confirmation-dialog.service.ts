import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface ConfirmationDialogOptions {
  title?: string;
  message: string;
  confirmButtonText?: string;
  cancelButtonText?: string;
  isDangerous?: boolean;
}

export interface DialogState {
  isVisible: boolean;
  options: ConfirmationDialogOptions;
}

@Injectable({
  providedIn: 'root',
})
export class ConfirmationDialogService {
  private dialogState$ = new BehaviorSubject<DialogState>({
    isVisible: false,
    options: {
      message: '',
    },
  });

  private resolveCallback?: (value: boolean) => void;

  public getDialogState(): Observable<DialogState> {
    return this.dialogState$.asObservable();
  }

  public getCurrentState(): DialogState {
    return this.dialogState$.value;
  }

  public show(options: ConfirmationDialogOptions): Promise<boolean> {
    return new Promise((resolve) => {
      this.resolveCallback = resolve;
      this.dialogState$.next({
        isVisible: true,
        options: {
          title: 'Confirm Action',
          confirmButtonText: 'Confirm',
          cancelButtonText: 'Cancel',
          isDangerous: false,
          ...options,
        },
      });
    });
  }

  public confirm(): void {
    if (this.resolveCallback) {
      this.resolveCallback(true);
    }
    this.hide();
  }

  public cancel(): void {
    if (this.resolveCallback) {
      this.resolveCallback(false);
    }
    this.hide();
  }

  private hide(): void {
    this.dialogState$.next({
      isVisible: false,
      options: {
        message: '',
      },
    });
  }
}

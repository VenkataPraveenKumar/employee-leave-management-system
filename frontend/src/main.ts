import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

/**
 * Bootstraps the root standalone AppComponent with centralized application configuration and zone tracking.
 */
bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));

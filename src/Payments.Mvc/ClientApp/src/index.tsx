import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter, Switch, Route } from 'react-router-dom';

import './css/site.scss';
import './polyfills/array.js';
import * as dateFns from 'date-fns';

import { CreateInvoicePage } from './pages/CreateInvoice';
import { EditInvoicePage } from './pages/EditInvoice';
import { PayInvoicePage } from './pages/PayInvoice';
import { PreviewRechargeInvoicePage } from './pages/PreviewRechargeInvoice';

declare let window: any;

interface ExtraWindow extends Window {
  dateFns: any;
}

const extraWindow = (window as unknown) as ExtraWindow;

extraWindow.dateFns = {
  format: dateFns.format,

  getMonth: dateFns.getMonth,
  setMonth: dateFns.setMonth,

  getYear: dateFns.getYear,
  setYear: dateFns.setYear
};

const rootElement = document.getElementById('root');

if (rootElement) {
  // <React.StrictMode> should be used when possible.  ReactStrap will need to update context API usage first
  ReactDOM.render(
    <BrowserRouter>
      <React.Fragment>
        <Switch>
          {/* Match any server-side routes and send empty content to let MVC return the view details */}
          <Route path='/:team/Invoices/Create' component={CreateInvoicePage} />
          <Route path='/:team/Invoices/Edit' component={EditInvoicePage} />
          <Route path='/Recharge/Pay/:id?' component={PayInvoicePage} />
          <Route
            path='/:team/Recharge/Preview/:id?'
            component={PreviewRechargeInvoicePage}
          />
        </Switch>
      </React.Fragment>
    </BrowserRouter>,
    rootElement
  );
}

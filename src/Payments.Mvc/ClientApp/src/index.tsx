import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

import './css/site.scss';
import './polyfills/array.js';
import { format, getMonth, setMonth, getYear, setYear } from 'date-fns';

import { CreateInvoicePage } from './pages/CreateInvoice';
import { EditInvoicePage } from './pages/EditInvoice';
import { PayInvoicePage } from './pages/PayInvoice';
import { FinancialApproveInvoicePage } from './pages/FinancialApproveInvoice';
import { PreviewRechargeInvoicePage } from './pages/PreviewRechargeInvoice';

declare let window: any;

interface ExtraWindow extends Window {
  dateFns: any;
}

const extraWindow = (window as unknown) as ExtraWindow;

extraWindow.dateFns = {
  format,
  getMonth,
  setMonth,
  getYear,
  setYear
};

const rootElement = document.getElementById('root');

if (rootElement) {
  const root = createRoot(rootElement);
  
  // <React.StrictMode> should be used when possible.  ReactStrap will need to update context API usage first
  root.render(
    <BrowserRouter>
      <Routes>
        {/* Match any server-side routes and send empty content to let MVC return the view details */}
        <Route path='/:team/Invoices/Create' element={<CreateInvoicePage />} />
        <Route path='/:team/Invoices/Edit/:id?' element={<EditInvoicePage />} />
        <Route path='/Recharge/Pay/:id?' element={<PayInvoicePage />} />
        <Route
          path='/Recharge/FinancialApprove/:id?'
          element={<FinancialApproveInvoicePage />}
        />
        <Route
          path='/:team/Recharge/Preview/:id?'
          element={<PreviewRechargeInvoicePage />}
        />
      </Routes>
    </BrowserRouter>
  );
}

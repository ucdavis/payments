import * as React from 'react';
import TeamContext from '../contexts/TeamContext';

import EditInvoiceContainer from '../containers/EditInvoiceContainer';

import { Account } from '../models/Account';
import { Coupon } from '../models/Coupon';
import { EditInvoice } from '../models/EditInvoice';
import { Team } from '../models/Team';

declare var accounts: Account[];
declare var coupons: Coupon[];
declare var id: number;
declare var model: EditInvoice;
declare var sent: boolean;
declare var team: Team;

export const EditInvoicePage = () => (
  <TeamContext.Provider value={team}>
    <EditInvoiceContainer
      accounts={accounts}
      coupons={coupons}
      id={id}
      invoice={model}
      sent={sent}
      team={team}
    />
  </TeamContext.Provider>
);

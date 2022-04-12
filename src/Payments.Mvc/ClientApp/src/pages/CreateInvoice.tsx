import * as React from 'react';

import TeamContext from '../contexts/TeamContext';

import CreateInvoiceContainer from '../containers/CreateInvoiceContainer';

import { Account } from '../models/Account';
import { Coupon } from '../models/Coupon';
import { Team } from '../models/Team';

declare var accounts: Account[];
declare var coupons: Coupon[];
declare var team: Team;

export const CreateInvoicePage = () => (
  <TeamContext.Provider value={team}>
    <CreateInvoiceContainer accounts={accounts} coupons={coupons} team={team} />
  </TeamContext.Provider>
);
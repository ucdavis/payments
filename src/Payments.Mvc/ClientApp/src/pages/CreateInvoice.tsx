import * as React from 'react'; 
import * as ReactDOM from 'react-dom'; 
import { AppContainer } from 'react-hot-loader';

import TeamContext from '../contexts/TeamContext';

import CreateInvoiceContainer from '../containers/CreateInvoiceContainer';

import { Account } from '../models/Account';
import { Coupon } from '../models/Coupon';
import { Team } from '../models/Team';

declare var accounts: Account[];
declare var coupons: Coupon[];
declare var team: Team;

function renderApp() { 
    // This code starts up the React app when it runs in a browser. It sets up the routing 
    // configuration and injects the app into a DOM element.
    ReactDOM.render(
        <AppContainer>
            <TeamContext.Provider value={team}>
                <CreateInvoiceContainer
                    accounts={accounts}
                    coupons={coupons}
                    team={team}
                />
            </TeamContext.Provider>
        </AppContainer>, 
        document.getElementById('react-app') 
    ); 
} 
 
renderApp(); 
 
// Allow Hot Module Replacement 
if (module.hot) { 
    module.hot.accept('../containers/CreateInvoiceContainer', () => { 
        renderApp(); 
    }); 
} 
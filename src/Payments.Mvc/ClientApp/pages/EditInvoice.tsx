import * as React from 'react'; 
import * as ReactDOM from 'react-dom'; 
import { AppContainer } from 'react-hot-loader';

import EditInvoiceContainer from '../containers/EditInvoiceContainer';

import { Account } from '../models/Account';
import { Invoice } from '../models/Invoice';
import { Team } from '../models/Team';

declare var accounts: Account[]
declare var id: number;
declare var model: Invoice;
declare var sent: boolean;
declare var team: Team;

function renderApp() { 
    // This code starts up the React app when it runs in a browser. It sets up the routing 
    // configuration and injects the app into a DOM element.
    ReactDOM.render( 
        <AppContainer>
            <EditInvoiceContainer
                accounts={accounts}
                id={id}
                invoice={model}
                sent={sent}
                team={team}
            />
        </AppContainer>, 
        document.getElementById('react-app') 
    ); 
} 
 
renderApp(); 
 
// Allow Hot Module Replacement 
if (module.hot) { 
    module.hot.accept('../containers/EditInvoiceContainer', () => { 
        renderApp(); 
    }); 
} 
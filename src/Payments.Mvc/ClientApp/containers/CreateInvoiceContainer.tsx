import * as React from 'react';

import "isomorphic-fetch";

import { Account } from '../models/Account';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceItem } from '../models/InvoiceItem';
import { Team } from '../models/Team';

import AccountSelectControl from '../components/accountSelectControl';
import Alert from '../components/alert';
import DueDateControl from '../components/dueDateControl';
import EditItemsTable from '../components/editItemsTable';
import LoadingModal from '../components/loadingModal';
import MemoInput from '../components/memoInput';
import MultiCustomerControl from '../components/multiCustomerControl';
import SendModal from '../components/sendModal';

declare var antiForgeryToken: string;

interface IProps {
    accounts: Account[];
    team: Team;
}

interface IState {
    ids: number[] | undefined;
    accountId: number;
    customers: InvoiceCustomer[];
    dueDate: string;
    discount: number;
    taxPercent: number;
    memo: string;
    items: InvoiceItem[];
    loading: boolean;
    errorMessage: string;

    isSendModalOpen: boolean;
}

export default class CreateInvoiceContainer extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);

        const defaultAccount = props.accounts.find(a => a.isDefault);

        this.state = {
            accountId: defaultAccount ? defaultAccount.id : 0,
            customers: [],
            discount: 0,
            dueDate: '',
            ids: undefined,
            items: [{
                amount: 0,
                description: '',
                id: 0,
                quantity: 0,
            }],
            memo: '',
            taxPercent: 0,
            
            errorMessage: '',
            loading: false,
            isSendModalOpen: false,
        };
    }

    public render() {
        const { accounts, team } = this.props;
        const { accountId, dueDate, items, discount, taxPercent, customers, memo, loading } = this.state;
        
        return (
            <div className="card-style">
                <LoadingModal loading={loading} />
                <div className="card-header-yellow card-bot-border">
                    <div className="card-head">
                        <h2>Create Invoice for { team.name }</h2>
                    </div>
                </div>
                <div className="card-content invoice-customer">
                    <MultiCustomerControl
                        customers={customers}
                        onChange={(c) => this.updateProperty('customers', c)}
                    />
                </div>
                <div className="card-content invoice-items">
                    <h2>Invoice Items</h2>
                    <EditItemsTable
                        items={items}
                        discount={discount}
                        taxPercent={taxPercent}
                        onItemsChange={(v) => this.updateProperty('items', v)}
                        onDiscountChange={(v) => this.updateProperty('discount', v)}
                        onTaxPercentChange={(v) => this.updateProperty('taxPercent', v)}
                    />
                </div>
                <div className="card-content invoice-memo">
                    <h2>Memo</h2>
                    <div className="form-group">
                        <MemoInput value={memo} onChange={(v) => this.updateProperty('memo', v)} />
                    </div>
                </div>
                <div className="card-content invoice-billing">
                    <h2>Billing</h2>
                    <div className="form-group">
                        <label>Due Date?</label>
                        <DueDateControl value={dueDate} onChange={(d) => this.updateProperty('dueDate', d)} />
                    </div>
                    <div className="form-group">
                        <label>Income Account</label>
                        <AccountSelectControl accounts={accounts} value={accountId} onChange={(a) => this.updateProperty('accountId', a)} />
                    </div>
                </div>
                <div className="card-foot invoice-action">
                    <div className="row flex-between flex-center">
                        <div className="col">
                            <button className="btn-plain color-unitrans">Cancel</button>
                        </div>
                        <div className="col d-flex justify-content-center align-items-end">
                            { this.renderError() }
                        </div>
                        <div className="col d-flex justify-content-end align-items-baseline">
                            <button className="btn-plain mr-3" onClick={this.onSubmit}>Save and close</button>
                            <button className="btn" onClick={this.openSendModal}>Send ...</button>
                            { this.renderSendModal() }
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    private renderSendModal() {
        const { team } = this.props;
        const { dueDate, customers, discount, taxPercent, items, memo, isSendModalOpen } = this.state;

        let emailLine = '';
        if (customers.length > 1) {
            emailLine = 'Multiple Customers';
        } else if (customers.length > 0) {
            emailLine = customers[0].email;
        }

        const invoice = {
            customer: { email: emailLine },
            discount,
            dueDate: dueDate ? new Date(dueDate) : undefined,
            items,
            memo,
            taxPercent,
        };

        return (
            <SendModal
                isModalOpen={isSendModalOpen}
                invoice={invoice}
                team={team}
                onCancel={() => { this.setState({ isSendModalOpen: false}) }}
                onSend={this.onSend}
            />
        );
    }

    private renderError() {
        const { errorMessage } = this.state;
        if (!errorMessage) {
            return null;
        }

        return (
            <Alert className="flex-grow-1 alert-danger" onDismiss={this.dismissErrorMessage}>
                <strong className="mr-3">Error!</strong>
                <span>{ errorMessage }</span>
            </Alert>
        );
    }

    private updateProperty = (name: any, value: any) => {
        this.setState({
            [name]: value,
        });
    }

    private saveInvoice = async () => {
        const { slug } = this.props.team;
        const { accountId, dueDate, customers, discount, taxPercent, items, memo } = this.state;

        // create submit object
        const invoice = {
            accountId,
            customers,
            discount,
            dueDate,
            items,
            memo,
            taxPercent,
        };

        // create url
        const url = `/${slug}/invoices/create`;

        // fetch
        const response = await fetch(url, {
            body: JSON.stringify(invoice),
            credentials: "same-origin",
            headers: new Headers({
                "Content-Type": "application/json",
                "RequestVerificationToken": antiForgeryToken
            }),
            method: "POST",
        });

        const result = await response.json();
        if (result.success) {
            this.setState({ ids: result.ids });
            return true;
        }

        this.setState({
            errorMessage: result.errorMessage
        })
        return false;
    }

    private sendInvoices = async () => {
        const { slug } = this.props.team;
        const { ids } = this.state;
        if (ids === undefined) {
            this.setState({
                errorMessage: "Could not send email. Try to Save and Close instead."
            })
            return false;
        }

        // tslint:disable-next-line:prefer-for-of
        for (let i = 0; i < ids.length; i++) {
            const id = ids[i];
            // send invoice
            const url = `/${slug}/send/${id}`;

            const response = await fetch(url, {
                credentials: "same-origin",
                headers: new Headers({
                    "Content-Type": "application/json",
                    "RequestVerificationToken": antiForgeryToken
                }),
                method: "POST",
            });
            
            const result = await response.json();
            if (!result.success) {
                this.setState({
                    errorMessage: result.errorMessage
                });
                return false;
            }
        }
        
        return true;
    }

    private onSubmit = async () => {
        const { slug } = this.props.team;
        this.setState({ loading: true });

        // save invoice
        const saveResult = await this.saveInvoice();
        if (!saveResult) {
            this.setState({ loading: false });
            return;
        }

        // return to all invoices page
        window.location.pathname = `/${slug}/invoices`;
    }

    private openSendModal = () => {
        this.setState({
            isSendModalOpen: true,
        })
    }

    private onSend = async (ccEmails) => {
        const { slug } = this.props.team;
        this.setState({ loading: true });

        // save invoice
        const saveResult = await this.saveInvoice();
        if (!saveResult) {
            this.setState({ loading: false });
            return;
        }

        // send emails
        const sendResult = await this.sendInvoices();
        // a failure here means that the invoices are saved, just not all sent
        // send user back to invoices page with error message
        if (!sendResult) {
            // return to all invoices page
            window.location.pathname = `/${slug}/invoices`;
            return;
        }

        // return to all invoices page
        window.location.pathname = `/${slug}/invoices`;
    }

    private dismissErrorMessage = () => {
        this.setState({ errorMessage: "" });
    }
}

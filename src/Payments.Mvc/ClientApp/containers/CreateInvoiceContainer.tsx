import "isomorphic-fetch";
import * as React from 'react';
import { Account } from '../models/Account';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceItem } from '../models/InvoiceItem';
import { Team } from '../models/Team';

import AccountSelectControl from '../components/accountSelectControl';
import DueDateControl from '../components/dueDateControl';
import EditItemsTable from '../components/editItemsTable';
import LoadingModal from '../components/loadingModal';
import MemoInput from '../components/memoInput';
import MultiCustomerControl from '../components/multiCustomerControl';

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
    taxRate: number;
    memo: string;
    items: InvoiceItem[];
    loading: boolean;
    errorMessage: string;
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
            errorMessage: '',
            ids: undefined,
            items: [{
                amount: 0,
                description: '',
                id: 0,
                quantity: 0,
            }],
            loading: false,
            memo: '',
            taxRate: 0,
        };
    }

    public render() {
        const { accounts, team } = this.props;
        const { accountId, dueDate, items, discount, taxRate, customers, memo, loading } = this.state;
        
        return (
            <div className="card-style">
                <LoadingModal loading={loading} />
                <div className="card-header-yellow card-bot-border">
                    <div className="card-head">
                        <h1>Create Invoice for { team.name }</h1>
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
                        taxRate={taxRate}
                        onItemsChange={(v) => this.updateProperty('items', v)}
                        onDiscountChange={(v) => this.updateProperty('discount', v)}
                        onTaxRateChange={(v) => this.updateProperty('taxRate', v)}
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
                        <DueDateControl value={dueDate} onChange={(d) => this.updateProperty('dueDate', d)} />
                    </div>
                    <div className="form-group">
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
                        <div className="col d-flex justify-content-end align-items-center">
                            <button className="btn-plain" onClick={this.onSubmit}>Save and close</button>
                            <button className="btn" onClick={this.onSend}>Save and Send</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    private renderError() {
        const { errorMessage } = this.state;
        if (!errorMessage) {
            return null;
        }

        return (
            <div className="flex-grow-1 alert alert-danger" role="alert">
                <strong>Error!</strong> { errorMessage }
                <button type="button" className="close" aria-label="Close" onClick={this.dismissErrorMessage}>
                    <i className="fa fa-times" />
                </button>
            </div>
        );
    }

    private updateProperty = (name: any, value: any) => {
        this.setState({
            [name]: value,
        });
    }

    private saveInvoice = async () => {
        const { slug } = this.props.team;
        const { accountId, dueDate, customers, discount, taxRate, items, memo } = this.state;

        // create submit object
        const invoice = {
            accountId,
            customers,
            discount,
            dueDate,
            items,
            memo,
            tax: taxRate,
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

    private onSend = async () => {
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

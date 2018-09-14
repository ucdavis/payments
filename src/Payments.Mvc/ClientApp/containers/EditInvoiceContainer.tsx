import * as React from 'react';

import { format } from 'date-fns';
import "isomorphic-fetch";

import { Account } from '../models/Account';
import { Invoice } from '../models/Invoice';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceItem } from '../models/InvoiceItem';
import { Team } from '../models/Team';

import AccountSelectControl from '../components/accountSelectControl';
import Alert from '../components/alert';
import CustomerControl from '../components/customerControl';
import DueDateControl from '../components/dueDateControl';
import EditItemsTable from '../components/editItemsTable';
import InvoiceForm from '../components/invoiceForm';
import LoadingModal from '../components/loadingModal';
import MemoInput from '../components/memoInput';
import SendModal from '../components/sendModal';

declare var antiForgeryToken: string;

interface IProps {
    accounts: Account[];
    id: number;
    invoice: Invoice;
    sent: boolean;
    team: Team;
}

interface IState {
    accountId: number;
    customer: InvoiceCustomer;
    discount: number;
    dueDate: string;
    taxPercent: number;
    memo: string;
    items: InvoiceItem[];
    loading: boolean;
    errorMessage: string;
    isSendModalOpen: boolean;
    validate: boolean;
}

export default class EditInvoiceContainer extends React.Component<IProps, IState> {
    private _formRef: HTMLFormElement;

    constructor(props: IProps) {
        super(props);

        const { invoice } = this.props;

        // require at least one item
        const items = invoice.items;
        if (!items || items.length < 1) {
            items.push({
                amount: 0,
                description: '',
                id: 0,
                quantity: 0,
            });
        }

        this.state = {
            accountId: invoice.accountId,
            customer: invoice.customer,
            discount: invoice.discount || 0,
            dueDate: invoice.dueDate ? format(invoice.dueDate, 'MM/DD/YYYY') : '',
            items,
            memo: invoice.memo,
            taxPercent: invoice.taxPercent || 0,
            
            errorMessage: "",
            loading: false,
            isSendModalOpen: false,
            validate: false,
        };
    }

    public render() {
        const { id, sent, team, accounts } = this.props;
        const { accountId, customer, dueDate, items, discount, taxPercent, memo, loading, validate } = this.state;
        

        return (
            <InvoiceForm className="card-style" validate={validate} formRef={r => this._formRef = r}>
                <LoadingModal loading={loading} />
                <div className="card-header-yellow card-bot-border">
                    <div className="card-head">
                        <h2>Edit Invoice #{ id } for { team.name } </h2>
                    </div>
                </div>
                <div className="card-content invoice-customer">
                    <h3>Customer Info</h3>
                    <div className="form-group">
                        <label>Customer Email</label>
                        <CustomerControl
                            customer={customer}
                            onChange={(c) => { this.updateProperty("customer", c) }}
                        />
                        <div className="invalid-feedback">
                            Customer required.
                        </div>
                    </div>
                </div>
                <div className="card-content invoice-items">
                    <h3>Invoice Items</h3>
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
                    <h3>Memo</h3>
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
                        <div className="col d-flex justify-content-end align-items-center">
                            <button className="btn-plain mr-3" onClick={this.onSubmit}>Save and close</button>
                            { !sent &&
                                <button className="btn" onClick={this.openSendModal}>Send ...</button> }
                            { !sent && 
                                this.renderSendModal() }
                        </div>
                    </div>
                </div>
            </InvoiceForm>
        );
    }

    private renderSendModal() {
        const { team } = this.props;
        const { dueDate, customer, discount, taxPercent, items, memo, isSendModalOpen } = this.state;

        const invoice = {
            customer,
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
            [name]: value
        });
    }

    private saveInvoice = async () => {
        // enable validation
        this.setState({ validate: true });

        // check validation
        const isValid = this._formRef.checkValidity();
        if (!isValid) {
            return false;
        }

        const { id } = this.props;
        const { slug } = this.props.team;
        const { accountId, customer, discount, dueDate, taxPercent, items, memo } = this.state;

        // create submit object
        const invoice = {
            accountId,
            customer,
            discount,
            dueDate,
            items,
            memo,
            taxPercent,
        };

        // create save url
        const url = `/${slug}/invoices/edit/${id}`;

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
            return true;
        }

        this.setState({
            errorMessage: result.errorMessage
        })
        return false;
    }

    private sendInvoice = async () => {
        // send invoice
        const { id } = this.props;
        const { slug } = this.props.team;

        const url = `/${slug}/invoices/send/${id}`;

        const response = await fetch(url, {
            credentials: "same-origin",
            headers: new Headers({
                "Content-Type": "application/json",
                "RequestVerificationToken": antiForgeryToken
            }),
            method: "POST",
        });
        
        const result = await response.json();
        if (result.success) {
            return true;
        }

        this.setState({
            errorMessage: result.errorMessage
        })
        return false;
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

    private onSend = async () => {
        const { slug } = this.props.team;
        this.setState({ loading: true });

        // save invoice
        const saveResult = await this.saveInvoice();
        if (!saveResult) {
            this.setState({ loading: false });
            return;
        }

        const sendResult = await this.sendInvoice();
        if (!sendResult) {
            this.setState({ loading: false });
            return;
        }

        // return to all invoices page
        window.location.pathname = `/${slug}/invoices`;
    }

    private dismissErrorMessage = () => {
        this.setState({ errorMessage: "" });
    }
}

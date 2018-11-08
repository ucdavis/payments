import * as React from 'react';

import { format } from 'date-fns';
import "isomorphic-fetch";

import { uuidv4 } from "../utils/string";

import { Account } from '../models/Account';
import { Coupon } from '../models/Coupon';
import { EditInvoice } from '../models/EditInvoice';
import { InvoiceAttachment } from '../models/InvoiceAttachment';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceDiscount } from '../models/InvoiceDiscount';
import { InvoiceItem } from '../models/InvoiceItem';
import { PreviewInvoice } from '../models/PreviewInvoice';
import { Team } from '../models/Team';

import AccountSelectControl from '../components/accountSelectControl';
import Alert from '../components/alert';
import AttachmentsControl from '../components/attachmentsControl';
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
    coupons: Coupon[];
    id: number;
    invoice: EditInvoice;
    sent: boolean;
    team: Team;
}

interface IState {
    accountId: number;
    attachments: InvoiceAttachment[],
    customer: InvoiceCustomer;
    discount: InvoiceDiscount;
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

        // assign random ids
        const items = invoice.items || [];
        items.forEach(i => i.id = uuidv4())

        // require at least one item
        if (!items || items.length < 1) {
            items.push({
                amount: 0,
                description: '',
                id: uuidv4(),
                quantity: 0,
            });
        }

        this.state = {
            accountId: invoice.accountId,
            attachments: invoice.attachments,
            customer: invoice.customer,
            discount: {
                couponId: invoice.couponId,
                hasDiscount: !!(invoice.couponId || invoice.discount),
                maunalAmount: invoice.discount,
            },
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
        const { id, sent, team, accounts, coupons } = this.props;
        const { accountId, attachments, customer, discount, dueDate, items, taxPercent, memo, loading, validate } = this.state;
        

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
                    <CustomerControl
                        customer={customer}
                        onChange={(c) => { this.updateProperty("customer", c) }}
                    />
                </div>
                <div className="card-content invoice-items">
                    <h3>Invoice Items</h3>
                    <EditItemsTable
                        items={items}
                        coupons={coupons}
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
                <div className="card-content invoice-attachments">
                    <h2>Attachments</h2>
                    <AttachmentsControl
                        attachments={attachments}
                        onChange={(v) => this.updateProperty('attachments', v)} />
                </div>
                <div className="card-foot invoice-action">
                    <div className="row justify-content-between align-items-center">
                        <div className="col">
                            <button className="btn-plain color-unitrans" onClick={this.onCancel}>Cancel</button>
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
        const { attachments, dueDate, customer, discount, taxPercent, items, memo, isSendModalOpen } = this.state;

        const calculatedDiscount = !!discount.getCalculatedDiscount && discount.getCalculatedDiscount()

        const invoice: PreviewInvoice = {
            attachments,
            couponId: discount.couponId,
            customer,
            discount: calculatedDiscount,
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

    private onCancel = () => {
        window.history.go(-1);
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
        const { accountId, attachments, customer, discount, dueDate, taxPercent, items, memo } = this.state;

        const calculatedDiscount = !!discount.getCalculatedDiscount && discount.getCalculatedDiscount()

        // create submit object
        const invoice: EditInvoice = {
            accountId,
            attachments,
            couponId: discount.couponId,
            customer,
            discount: calculatedDiscount,
            dueDate: new Date(dueDate),
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

import * as React from 'react';

import "isomorphic-fetch";

import { uuidv4 } from "../utils/string";

import { Account } from '../models/Account';
import { Coupon } from '../models/Coupon';
import { CreateInvoice } from '../models/CreateInvoice';
import { InvoiceAttachment } from '../models/InvoiceAttachment';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceDiscount } from '../models/InvoiceDiscount';
import { InvoiceItem } from '../models/InvoiceItem';
import { PreviewInvoice } from '../models/PreviewInvoice';
import { Team } from '../models/Team';

import AccountSelectControl from '../components/accountSelectControl';
import Alert from '../components/alert';
import AttachmentsControl from '../components/attachmentsControl';
import DueDateControl from '../components/dueDateControl';
import EditItemsTable from '../components/editItemsTable';
import InvoiceForm from '../components/invoiceForm';
import LoadingModal from '../components/loadingModal';
import MemoInput from '../components/memoInput';
import MultiCustomerControl from '../components/multiCustomerControl';
import SendModal from '../components/sendModal';

declare var antiForgeryToken: string;


interface IProps {
    accounts: Account[];
    coupons: Coupon[];
    team: Team;
}

interface IState {
    ids: number[] | undefined;
    accountId: number;
    attachments: InvoiceAttachment[],
    customers: InvoiceCustomer[];
    discount: InvoiceDiscount;
    dueDate: string;
    taxPercent: number;
    memo: string;
    items: InvoiceItem[];
    loading: boolean;
    errorMessage: string;
    validate: boolean;

    isSendModalOpen: boolean;
}

export default class CreateInvoiceContainer extends React.Component<IProps, IState> {
    private _formRef: HTMLFormElement;

    constructor(props: IProps) {
        super(props);

        const defaultAccount = props.accounts.find(a => a.isDefault);

        this.state = {
            accountId: defaultAccount ? defaultAccount.id : 0,
            attachments: [],
            customers: [{
                address: '',
                email: '',
                name: ''
            }],
            discount: {
                hasDiscount: false,
            },
            dueDate: '',
            ids: undefined,
            items: [{
                amount: 0,
                description: '',
                id: 1,
                quantity: 0,
                taxExempt: false,
            }],
            memo: '',
            taxPercent: 0,
            
            errorMessage: '',
            loading: false,
            isSendModalOpen: false,
            validate: false,
        };
    }

    public render() {
        const { accounts, coupons, team } = this.props;
        const { accountId, attachments, discount, dueDate, items, taxPercent, customers, memo, loading, validate } = this.state;
        
        return (
            <InvoiceForm className="card-style" validate={validate} formRef={r => this._formRef = r}>
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
                    <div className="invalid-feedback">
                        Customer required.
                    </div>
                </div>
                <div className="card-content invoice-items">
                    <h2>Invoice Items</h2>
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
                        <div className="col d-flex justify-content-end align-items-baseline">
                            <button className="btn-plain mr-3" onClick={this.onSubmit}>Save and close</button>
                            <button className="btn" onClick={this.openSendModal}>Send ...</button>
                            { this.renderSendModal() }
                        </div>
                    </div>
                </div>
            </InvoiceForm>
        );
    }

    private renderSendModal() {
        const { coupons, team } = this.props;
        const { attachments, dueDate, customers, discount, taxPercent, items, memo, isSendModalOpen } = this.state;

        if (!isSendModalOpen) {
            return null;
        }

        let customer: InvoiceCustomer;
        if (customers.length > 1) {
            customer = { address: '', email: 'Multiple Customers', name: 'Multiple Customers' };
        } else {
            customer = customers[0];
        }

        const calculatedDiscount = !!discount.getCalculatedDiscount && discount.getCalculatedDiscount()

        const coupon = coupons.find(c => c.id === discount.couponId);

        const invoice: PreviewInvoice = {
            attachments,
            coupon,
            customerEmail: customer.email,
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
            [name]: value,
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


        const { slug } = this.props.team;
        const { accountId, attachments, discount, dueDate, customers, taxPercent, items, memo } = this.state;

        const calculatedDiscount = !!discount.getCalculatedDiscount ? discount.getCalculatedDiscount() : 0;

        // create submit object
        const invoice: CreateInvoice = {
            accountId,
            attachments,
            couponId: discount.couponId,
            customers,
            discount: calculatedDiscount,
            dueDate: new Date(dueDate),
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

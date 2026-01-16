import * as React from 'react';
import cs from 'classnames';

interface IProps {
    className?: string;
    validate: boolean;
    formRef?: (form: HTMLFormElement) => void;
    children?: React.ReactNode;
}

export default class InvoiceForm extends React.Component<IProps, {}> {
    public render() {
        const { formRef, validate } = this.props;

        const className = cs(this.props.className, {
            'needs-validation': !validate,
            'was-validated': validate,
        });

        return (
            <form
                className={className}
                onSubmit={e => e.preventDefault()}
                noValidate={!validate}
                ref={formRef}
            >
                {this.props.children}
            </form>
        );
    }
}

import * as React from 'react';
import cs from 'classnames';


interface IProps {
    className?: string;
    checked: boolean;
    onChange: (checked: boolean) => void;
    label?: string;
}

// uses https://lokesh-coder.github.io/pretty-checkbox

export default class Checkbox extends React.Component<IProps, {}> {
    public render() {
        const { className, checked, label } = this.props;

        const containerClass=cs(className, {
            'no-label': !label,
        });

        return (
            <div className={cs(containerClass, "pretty p-default p-curve p-thick p-pulse")}>
                <input type="checkbox" checked={checked} onChange={this.onChange} />
                <div className="state p-primary-o">
                    <label>{label}</label>
                </div>
            </div>
        );
    }

    private onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.props.onChange(e.target.checked);
    }
}
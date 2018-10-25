import * as React from 'react';
 
interface IProps {
    value: number;
    onChange: (value: number) => void;

    inputRef?: React.RefObject<HTMLInputElement>;
}

interface IState {
    type: string;
    value: string;

    // Fix for: https://bugzilla.mozilla.org/show_bug.cgi?id=1057858
    noopBlur: boolean;
}

export default class CurrencyControl extends React.PureComponent<IProps, IState> {

    constructor(props: IProps) {
        super(props);

        this.state = {
            type: "text",
            value: props.value.toFixed(2),

            noopBlur: false,
        };
    }

    public componentWillReceiveProps(nextProps: IProps) {
        this.setState({
            value: nextProps.value.toFixed(2),
        });
    }

    public render() {
        const { inputRef } = this.props;
        const { type, value } = this.state;

        return (
            <input
                type={type}
                min="0.01"
                step="0.01"
                className="form-control text-right"
                placeholder="0.00"
                value={value}
                onFocus={this.onFocus}
                onBlur={this.onBlur}
                onChange={(e) => { this.setState({ value: e.target.value }); }}
                required={true}
                ref={inputRef}
            />
        );
    }

    private onFocus = (event: React.FocusEvent<HTMLInputElement>) => {
        this.setState({noopBlur: true})
        this.setState({
            noopBlur: true,
            type: "number",
        });

        setTimeout(() => this.setState({noopBlur: false}), 100);
    }

    private onBlur = (event: React.FocusEvent<HTMLInputElement>) => {
        if (this.state.noopBlur) {
            return;
        }

        let discount = Number(event.target.value);
        if (isNaN(discount)) {
            discount = 0;
        }
        
        this.props.onChange(discount);
        this.setState({type: "text"});
    }
}
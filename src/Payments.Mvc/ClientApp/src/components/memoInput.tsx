import * as React from 'react';

interface IProps {
    value: string;
    onChange: (value: string) => void;
}

interface IState {
}

export default class MemoInput extends React.Component<IProps, IState> {

    constructor(props) {
        super(props);

        this.state = {
        };
    }

    public render() {
        const { value, onChange } = this.props;

        return (
            <textarea
                className="form-control"
                value={value}
                onChange={(e) => onChange(e.target.value)}
            />
        )
    }
}

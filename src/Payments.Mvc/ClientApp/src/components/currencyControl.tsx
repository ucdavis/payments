import * as React from 'react';
import NumberControl from './numberControl';

interface IProps {
  value: number;
  onChange: (value: number) => void;

  inputRef?: React.RefObject<HTMLInputElement>;
}

export default class CurrencyControl extends React.PureComponent<IProps, {}> {
  constructor(props: IProps) {
    super(props);
  }

  public render() {
    const { value, onChange, inputRef } = this.props;

    return (
      <NumberControl
        className='text-end'
        min={0.01}
        step={0.01}
        placeholder='0.00'
        value={value}
        onChange={onChange}
        required={true}
        inputRef={inputRef}
      />
    );
  }
}

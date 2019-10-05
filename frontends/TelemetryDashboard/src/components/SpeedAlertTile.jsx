import React from 'react'
import styled from 'styled-components'
import Axios from 'axios'
import {Card, Button} from 'react-bootstrap'
import Slider from '@material-ui/core/Slider'
import { toast } from 'react-toastify';
import { adalApiFetch } from '../adalConfig';

const SliderContainer = styled.div`
    margin-top: 50px;
`;

class SpeedAlertTile extends React.Component 
{
    constructor(props)
    {
        super(props)
        this.state = {
            currentSpeedAlertLimit: 80,
            showNotification: false
        }

        this.speedAlertValueChanged = this.speedAlertValueChanged.bind(this);
        this.saveSpeedAlert = this.saveSpeedAlert.bind(this);
    }

    async saveSpeedAlert()
    {
        adalApiFetch(Axios, process.env.REACT_APP_SERVICES_URL + "/SetSpeedAlert", {method: 'post'}).then(
            (response) => {
                console.log(response)
                toast.success("Your speed alert has been set successfully!", {
                    position: toast.POSITION.TOP_RIGHT
                })
            }
        ).catch((error) => {
            console.log(error)
            toast.error(`Your speed alert setup failed! (${error})`, {
                position: toast.POSITION.TOP_RIGHT
            })
        })
    }

    speedAlertValueChanged(event, value) {
        this.setState({
            currentSpeedAlertLimit: value
        })
    }

    render()
    {
        return (
            <Card>
                <Card.Body>
                    <Card.Title>Speed alert</Card.Title>
                    <Card.Subtitle>Setup your alert below.</Card.Subtitle>
                    <SliderContainer>
                        <Slider 
                            defaultValue={80}
                            aria-labelledby="discrete-slider-always"
                            valueLabelDisplay="on"
                            step={5}
                            min={50}
                            max={250}
                            onChangeCommitted={this.speedAlertValueChanged} />
                    </SliderContainer>
                    
                    <Button onClick={this.saveSpeedAlert} block>Enable alert at {this.state.currentSpeedAlertLimit}</Button>
                </Card.Body>
            </Card>
        )
    }
}

export default SpeedAlertTile;
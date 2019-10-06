import React from 'react'
import Axios from 'axios'
import {Button, ButtonToolbar, ButtonGroup, Modal, InputGroup, FormControl} from 'react-bootstrap'
import { toast } from 'react-toastify';
import { adalVehicleServicesFetch } from '../adalConfig';

class VehicleSelectorCreateModal extends React.Component 
{
    constructor(props)
    {
        super(props)
        this.state = {
            deviceId: "",
            name: ""
        }

        this.handleDeviceIdChangeEvent = this.handleDeviceIdChangeEvent.bind(this);
        this.handleNameChangeEvent = this.handleNameChangeEvent.bind(this);
        this.addVehicle = this.addVehicle.bind(this);
    }

    addVehicle() {
        if (this.state.name === "" || this.state.deviceId === "") {
            toast.warn("Please fill the DeviceId and Name field.", {
                position: toast.POSITION.TOP_RIGHT
            })
            return;
        }
        adalVehicleServicesFetch(Axios, "/AddDeviceIdUserPairing", {
            method: "post",
            data: this.state
        }).then((response) => {
            console.log(response)
            this.props.onHide();
        })
    }

    handleDeviceIdChangeEvent(event) {
        var temp = this.state
        if (this.state.deviceId === this.state.name) {
            temp.name = event.target.value
        }
        temp.deviceId = event.target.value

        this.setState(temp)
    }

    handleNameChangeEvent(event) {
        this.setState({
            name: event.target.value
        })
    }

    render()
    {
        return (
            <Modal {...this.props} size="lg" aria-labelledby="contained-modal-title-vcenter" centered >
                <Modal.Header closeButton>
                    <Modal.Title id="contained-modal-title-vcenter">
                        Add vehicle
                    </Modal.Title> 
                </Modal.Header>
                <Modal.Body>
                    <InputGroup className="mb-3">
                    <InputGroup.Prepend>
                        <InputGroup.Text>DeviceId and vehicle name</InputGroup.Text>
                    </InputGroup.Prepend>
                    <FormControl value={this.state.deviceId} onChange={this.handleDeviceIdChangeEvent}/>
                    <FormControl value={this.state.name} onChange={this.handleNameChangeEvent}/>
                    </InputGroup>
                </Modal.Body>
                <Modal.Footer>
                    <ButtonToolbar>
                        <ButtonGroup className="mx-2"><Button onClick={() => this.addVehicle()} variant="primary">Add vehicle</Button></ButtonGroup>        
                        <ButtonGroup><Button onClick={this.props.onHide} variant="danger">Close</Button></ButtonGroup>
                    </ButtonToolbar>
                </Modal.Footer>
            </Modal>
        )
    }
}

export default VehicleSelectorCreateModal;
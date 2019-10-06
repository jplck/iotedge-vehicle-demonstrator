import React from 'react'
import styled from 'styled-components'
import Axios from 'axios'
import {Card, Button, ButtonToolbar, Table, ButtonGroup} from 'react-bootstrap'
import { adalVehicleServicesFetch } from '../adalConfig';
import VehicleSelectorCreateModal from './VehicleSelectorCreateModal'

const TableContent = styled.div`
    font-size:10pt;
    word-break: break-word;
`;

class VehicleSelectorTile extends React.Component 
{
    constructor(props)
    {
        super(props)
        this.state = {
            addVehicleModalShow: false,
            pairings: [],
            selectedVehicle: null
        }
        this.selectVehicle = this.selectVehicle.bind(this);
    }

    showModal(show) {
        this.setState({
            addVehicleModalShow: show
        })

        if (!show) {
            this.reloadData();
        }
    }

    reloadData() {
        adalVehicleServicesFetch(Axios, "/GetDeviceIdUserPairings", {
            method: "get"
        }).then((response) => {
            this.setState({
                pairings: response.data
            })
        })
    }

    componentDidMount() {
        this.reloadData();
    }

    selectVehicle(pairing) {
        this.setState({
            selectedVehicle: pairing
        })
        this.props.onSelect(pairing)
    }

    render()
    {
        const styleSelected = {
            color: 'blue',
            fontWeight: 'bold'
        }

        const styleNotSelected = {
            color: 'black',
            fontWeight: 'normal'
        }

        return (
            <Card>
                <VehicleSelectorCreateModal show={this.state.addVehicleModalShow} onHide={() => this.showModal(false)}/>
                <Card.Body>
                    <Card.Title>Vehicle selector</Card.Title>
                    <TableContent>
                        <Table striped>
                            <thead>
                                <tr>
                                    <th>#</th>
                                    <th>DeviceId</th>
                                    <th>Name</th>
                                </tr>
                            </thead>
                            <tbody>
                                {this.state.pairings.map((pairing, index) => 
                                    <tr style={this.state.selectedVehicle !== null && pairing.deviceId === this.state.selectedVehicle.deviceId ? styleSelected : styleNotSelected} key={index} onClick={() => this.selectVehicle(pairing)}>
                                        <td>{index}</td>
                                        <td>{pairing.deviceId}</td>
                                        <td>{pairing.name}</td>
                                    </tr>
                                )}
                            </tbody>
                        </Table>
                    </TableContent>
                    <ButtonToolbar>
                        <ButtonGroup><Button onClick={() => this.showModal(true)} variant="primary">Add vehicle</Button></ButtonGroup>        
                        <ButtonGroup className="mx-2"><Button variant="danger">Remove</Button></ButtonGroup>
                    </ButtonToolbar>
                </Card.Body>
            </Card>
        )
    }
}

export default VehicleSelectorTile;
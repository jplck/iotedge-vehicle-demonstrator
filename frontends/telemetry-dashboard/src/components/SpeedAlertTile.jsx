import React from 'react'
import styled from 'styled-components'
import Axios from 'axios';

class SpeedAlertTile extends React.Component 
{
    constructor(props)
    {
        super(props)
        this.state = {
            currentSpeedAlertLimit: 80
        }
    }

    async setSpeedAlert()
    {
        const response = await Axios.post('https://vehicle-services.azurewebsites.net/api/SetSpeedAlert', null, {
            headers: {
                'Authorization': `Bearer ${this.props.authToken}`
            }
        }
        )
        console.log(response)
    }

    render()
    {
        return (
            <div><a href="#" onClick={() => this.setSpeedAlert()}>tile1</a></div>
        )
    }
}

export default SpeedAlertTile;
/*----------------------------------------------------------------------------*/
/* Copyright (c) 2017-2018 FIRST. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include <WPILib.h>
#include <iostream>
#include "ctre/Phoenix.h"

class Robot: public frc::IterativeRobot{
    ctre::phoenix::motorcontrol::can::WPI_TalonSRX m_leftMotor{0};
    ctre::phoenix::motorcontrol::can::WPI_TalonSRX m_rightMotor{1};
    frc::DifferentialDrive m_robotDrive{m_leftMotor, m_rightMotor};
    frc::Joystick m_stick{1};
    frc::Timer auto_timer;
    bool run_auto = true;

    bool driveMode = false;

public:
    void RobotInit(){}
    void DisabledInit(){}
    void TeleopInit(){}
    void TestInit(){}
    void RobotPeriodic(){}
    void DisabledPeriodic(){}
    void TestPeriodic(){}

    void AutonomousInit(){
        auto_timer.Start();
    }

    void AutonomousPeriodic(){
        //std::cout<<"Remaining: "<<auto_timer.Get()<<"\n";
        if(!auto_timer.HasPeriodPassed(5) && run_auto){
            m_robotDrive.TankDrive(1.0,-1.0);
            run_auto = false;
        } else{
            m_robotDrive.TankDrive(0,0);
        }
    }

    void TeleopPeriodic(){
        double start = frc::Timer::GetFPGATimestamp();
        std::cout << "1: "<< m_stick.GetRawAxis(1) << " 4: " << m_stick.GetRawAxis(4)<< "\n";
        if(!driveMode)
            m_robotDrive.ArcadeDrive(-m_stick.GetRawAxis(1), m_stick.GetRawAxis(4));
        else
            m_robotDrive.TankDrive(m_stick.GetRawAxis(1), m_stick.GetRawAxis(5));
        if(m_stick.GetRawButton(2)) {
            driveMode = !driveMode;
        }
        frc::Wait(0.005);
        //std::cout<<"Loop time: "<<(frc::Timer::GetFPGATimestamp() - start)<<" s\n";
    }
};

START_ROBOT_CLASS(Robot)

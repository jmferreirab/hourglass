﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceSoundPlayer.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass
{
    using System;
    using System.Windows.Threading;

    /// <summary>
    /// Plays <see cref="Sound"/>s stored in the assembly.
    /// </summary>
    public class ResourceSoundPlayer : IDisposable
    {
        #region Private Members

        /// <summary>
        /// A <see cref="System.Media.SoundPlayer"/> that can be used to play *.wav files.
        /// </summary>
        private readonly System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();

        /// <summary>
        /// A <see cref="DispatcherTimer"/> used to raise events.
        /// </summary>
        private readonly DispatcherTimer dispatcherTimer;

        /// <summary>
        /// Indicates whether this object has been disposed.
        /// </summary>
        private bool disposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceSoundPlayer"/> class.
        /// </summary>
        public ResourceSoundPlayer()
        {
            this.dispatcherTimer = new DispatcherTimer();
            this.dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            this.dispatcherTimer.Tick += this.DispatcherTimerTick;
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when sound playback has started.
        /// </summary>
        public event EventHandler PlaybackStarted;

        /// <summary>
        /// Raised when sound playback has stopped.
        /// </summary>
        public event EventHandler PlaybackStopped;

        /// <summary>
        /// Raised when sound playback has completed.
        /// </summary>
        public event EventHandler PlaybackCompleted;

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays a <see cref="Sound"/> asynchronously.
        /// </summary>
        /// <param name="sound">A <see cref="Sound"/>.</param>
        /// <param name="loop">A value indicating whether playback should be looped.</param>
        /// <returns><c>true</c> if the <see cref="Sound"/> plays successfully, or <c>false</c> otherwise.</returns>
        public bool Play(Sound sound, bool loop)
        {
            this.ThrowIfDisposed();

            // Stop all playback
            if (!this.Stop())
            {
                return false;
            }

            // Do not play nothing
            if (sound == null)
            {
                return true;
            }

            // Try to play the sound
            try
            {
                // Load the sound data
                this.soundPlayer.Stream = sound.GetStream();

                if (loop)
                {
                    // Asynchronously play looping sound
                    this.soundPlayer.PlayLooping();
                }
                else
                {
                    // Asynchronously play sound once
                    this.soundPlayer.Play();

                    // Start a timer to notify the completion of playback if we know the duration
                    if (sound.Duration.HasValue)
                    {
                        this.dispatcherTimer.Interval = sound.Duration.Value;
                        this.dispatcherTimer.Start();
                    }
                }
            }
            catch
            {
                return false;
            }

            // Raise an event
            this.OnPlaybackStarted();
            return true;
        }

        /// <summary>
        /// Stops playback of a <see cref="Sound"/> if playback is occurring.
        /// </summary>
        /// <returns><c>true</c> if playback is stopped successfully or no playback was occurring, or <c>false</c>
        /// otherwise.</returns>
        public bool Stop()
        {
            this.ThrowIfDisposed();

            try
            {
                // Stop playback and prevent a completion event
                this.soundPlayer.Stop();
                this.dispatcherTimer.Stop();

                // Dispose the stream to the sound data
                if (this.soundPlayer.Stream != null)
                {
                    this.soundPlayer.Stream.Dispose();
                    this.soundPlayer.Stream = null;
                }
            }
            catch
            {
                return false;
            }

            // Raise an event
            this.OnPlaybackStopped();
            return true;
        }

        /// <summary>
        /// Disposes the timer.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true /* disposing */);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Disposes the timer.
        /// </summary>
        /// <param name="disposing">A value indicating whether this method was invoked by an explicit call to <see
        /// cref="Dispose"/>.</param>
        protected void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            if (disposing)
            {
                this.soundPlayer.Stop();
                this.soundPlayer.Dispose();

                if (this.soundPlayer.Stream != null)
                {
                    this.soundPlayer.Stream.Dispose();
                }

                this.dispatcherTimer.Stop();
            }
        }

        /// <summary>
        /// Throws a <see cref="ObjectDisposedException"/> if the object has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        /// <summary>
        /// Raises the <see cref="PlaybackStarted"/> event.
        /// </summary>
        protected virtual void OnPlaybackStarted()
        {
            EventHandler eventHandler = this.PlaybackStarted;

            if (eventHandler != null)
            {
                eventHandler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the <see cref="PlaybackStopped"/> event.
        /// </summary>
        protected virtual void OnPlaybackStopped()
        {
            EventHandler eventHandler = this.PlaybackStopped;

            if (eventHandler != null)
            {
                eventHandler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the <see cref="PlaybackCompleted"/> event.
        /// </summary>
        protected virtual void OnPlaybackCompleted()
        {
            EventHandler eventHandler = this.PlaybackCompleted;

            if (eventHandler != null)
            {
                eventHandler(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Invoked when the <see cref="dispatcherTimer"/> interval has elapsed.
        /// </summary>
        /// <param name="sender">The <see cref="DispatcherTimer"/>.</param>
        /// <param name="e">The event data.</param>
        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            // Prevent multiple completion events
            this.dispatcherTimer.Stop();

            // Raise an event
            this.OnPlaybackCompleted();
        }

        #endregion
    }
}